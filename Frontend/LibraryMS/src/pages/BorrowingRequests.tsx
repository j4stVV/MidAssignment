import { useState, useEffect } from 'react';
import { BorrowingRequestDto, RequestStatus } from "@/models/BorrowingRequest";
import { getAllBorrowingRequests, approveBorrowingRequest, rejectBorrowingRequest } from '@/services/BorrowingService';
import { toast } from '@/components/ui/sonner';
import { Button } from '@/components/ui/button';
import { Badge } from '@/components/ui/badge';
import { Check, X, BookOpen, Clock, ChevronUp, ChevronDown } from 'lucide-react';
import { useAuth } from '@/contexts/AuthContext';

const BorrowingRequests = () => {
  const [requests, setRequests] = useState<BorrowingRequestDto[]>([]);
  const [filteredRequests, setFilteredRequests] = useState<BorrowingRequestDto[]>([]);
  const [filter, setFilter] = useState<'all' | 'waiting' | 'approved' | 'rejected'>('all');
  const [isLoading, setIsLoading] = useState(true);
  const [expandedRequests, setExpandedRequests] = useState<Set<string>>(new Set());
  const { user } = useAuth();

  useEffect(() => {
    if (!user) {
      console.error('No user found in AuthContext');
      setIsLoading(false);
      return;
    }
    console.log('User:', user);
    loadRequests();
  }, []);

  const normalizeStatus = (status: string | number): RequestStatus => {
    if (typeof status === 'number') {
      switch (status) {
        case 0: return RequestStatus.Approved;
        case 1: return RequestStatus.Rejected;
        case 2: return RequestStatus.Waiting;
        default: return RequestStatus.Waiting;
      }
    }
    const normalized = status.charAt(0).toUpperCase() + status.slice(1).toLowerCase();
    return RequestStatus[normalized as keyof typeof RequestStatus] || RequestStatus.Waiting;
  };

  const loadRequests = async () => {
    setIsLoading(true);
    try {
      const allRequests = await getAllBorrowingRequests();
      const normalizedRequests = allRequests.map(request => ({
        ...request,
        status: normalizeStatus(request.status),
        details: request.details || []
      }));
      setRequests(normalizedRequests);
      applyFilter(normalizedRequests, filter);
    } catch (error: any) {
      console.error('Error loading requests:', error);
      toast.error(error.response?.data?.Message || 'Failed to load borrowing requests');
    } finally {
      setIsLoading(false);
    }
  };

  const applyFilter = (requests: BorrowingRequestDto[], filterStatus: string) => {
    console.log('Applying filter:', filterStatus);
    if (filterStatus === 'all') {
      setFilteredRequests(requests);
    } else {
      const normalizedFilter = filterStatus.charAt(0).toUpperCase() + filterStatus.slice(1);
      setFilteredRequests(requests.filter(request =>
        request.status === normalizedFilter
      ));
    }
  };

  const handleFilterChange = (newFilter: 'all' | 'waiting' | 'approved' | 'rejected') => {
    console.log('Filter changed to:', newFilter);
    setFilter(newFilter);
    applyFilter(requests, newFilter);
  };

  const handleApproveRequest = async (requestId: string) => {
    if (!user) {
      toast.error('User not authenticated');
      return;
    }

    try {
      await approveBorrowingRequest(requestId);
      toast.success('Request approved successfully');
      await loadRequests();
    } catch (error: any) {
      console.error('Error approving request:', error);
      toast.error(error.response?.data?.Message || 'Failed to approve request');
    }
  };

  const handleRejectRequest = async (requestId: string) => {
    if (!user) {
      toast.error('User not authenticated');
      return;
    }

    try {
      await rejectBorrowingRequest(requestId);
      toast.success('Request rejected');
      await loadRequests();
    } catch (error: any) {
      console.error('Error rejecting request:', error);
      toast.error(error.response?.data?.Message || 'Failed to reject request');
    }
  };

  const toggleRequestDetails = (requestId: string) => {
    setExpandedRequests(prev => {
      const newSet = new Set(prev);
      if (newSet.has(requestId)) {
        newSet.delete(requestId);
      } else {
        newSet.add(requestId);
      }
      return newSet;
    });
  };

  const getStatusBadge = (status: RequestStatus) => {
    switch (status) {
      case RequestStatus.Approved:
        return <Badge className="bg-status-approved">Approved</Badge>;
      case RequestStatus.Rejected:
        return <Badge className="bg-status-rejected">Rejected</Badge>;
      case RequestStatus.Waiting:
        return <Badge className="bg-status-waiting">Waiting</Badge>;
      default:
        return <Badge className="bg-status-waiting">Unknown</Badge>;
    }
  };

  if (!user) {
    return (
      <div className="text-center py-12">
        <h3 className="text-lg font-medium">Authentication Required</h3>
        <p className="text-muted-foreground">Please log in to view borrowing requests.</p>
      </div>
    );
  }

  if (isLoading) {
    return (
      <div className="text-center py-12">
        <Clock className="mx-auto h-12 w-12 text-muted-foreground/50 animate-spin" />
        <p className="text-muted-foreground">Loading borrowing requests...</p>
      </div>
    );
  }

  return (
    <div>
      <div className="mb-8">
        <h1 className="text-3xl font-bold">Borrowing Requests</h1>
        <p className="text-muted-foreground">
          Approve or reject user borrowing requests
        </p>
      </div>

      <div className="mb-6 flex flex-col sm:flex-row justify-between gap-2">
        <div className="flex gap-2">
          <Button
            variant={filter === 'all' ? 'default' : 'outline'}
            size="sm"
            onClick={() => handleFilterChange('all')}
          >
            All
          </Button>
          <Button
            variant={filter === 'waiting' ? 'default' : 'outline'}
            size="sm"
            onClick={() => handleFilterChange('waiting')}
            className="flex items-center gap-1"
          >
            <Clock size={14} /> Waiting
          </Button>
          <Button
            variant={filter === 'approved' ? 'default' : 'outline'}
            size="sm"
            onClick={() => handleFilterChange('approved')}
            className="flex items-center gap-1"
          >
            <Check size={14} /> Approved
          </Button>
          <Button
            variant={filter === 'rejected' ? 'default' : 'outline'}
            size="sm"
            onClick={() => handleFilterChange('rejected')}
            className="flex items-center gap-1"
          >
            <X size={14} /> Rejected
          </Button>
        </div>
      </div>

      {filteredRequests.length === 0 ? (
        <div className="text-center py-12 bg-muted/30 rounded-lg border border-border">
          <Clock className="mx-auto h-12 w-12 text-muted-foreground/50" />
          <h3 className="mt-2 text-lg font-medium">No {filter !== 'all' ? filter : ''} requests found</h3>
          <p className="text-muted-foreground">
            {filter !== 'all'
              ? `There are no ${filter} borrowing requests to display.`
              : 'There are no borrowing requests to display.'}
          </p>
        </div>
      ) : (
        <div className="space-y-6">
          {filteredRequests.map(request => (
            <div key={request.id} className="bg-card rounded-lg shadow-sm overflow-hidden border">
              <div className="p-4 bg-muted/20 border-b">
                <div className="flex flex-col gap-2">
                  <div className="flex items-center flex-wrap gap-2">
                    <div className="flex items-center gap-2">
                      <h3 className="font-medium">Request #{request.id.slice(0, 8)}</h3>
                      {getStatusBadge(request.status)}
                    </div>
                    {request.status === RequestStatus.Waiting && (
                      <div className="flex gap-2 ml-auto">
                        <Button
                          variant="outline"
                          size="sm"
                          className="flex items-center gap-1"
                          onClick={() => handleRejectRequest(request.id)}
                        >
                          <X size={14} /> Reject
                        </Button>
                        <Button
                          size="sm"
                          className="flex items-center gap-1"
                          onClick={() => handleApproveRequest(request.id)}
                        >
                          <Check size={14} /> Approve
                        </Button>
                      </div>
                    )}
                  </div>
                  <p className="text-sm text-muted-foreground">
                    <span>From: <span className="font-medium">{request.requestorName || 'Unknown'}</span></span>
                    <span className="mx-2">•</span>
                    <span>{new Date(request.requestedDate).toLocaleDateString()}</span>
                  </p>
                  <div className="flex justify-end">
                    <Button
                      variant="ghost"
                      size="sm"
                      onClick={() => toggleRequestDetails(request.id)}
                      className="p-1"
                    >
                      {expandedRequests.has(request.id) ? (
                        <ChevronUp size={16} />
                      ) : (
                        <ChevronDown size={16} />
                      )}
                    </Button>
                  </div>
                </div>
              </div>

              {expandedRequests.has(request.id) && (
                <div className="p-4">
                  <h4 className="text-sm font-medium mb-2">Books requested:</h4>
                  <div className="space-y-2">
                    {(request.details || []).map(detail => (
                      <div key={detail.id} className="flex items-center gap-3 py-2 border-b last:border-0">
                        <div className="w-8 h-8 rounded bg-primary/10 text-primary flex items-center justify-center">
                          <BookOpen size={16} />
                        </div>
                        <div>
                          <p className="font-medium">{detail.bookTitle || 'Unknown Book'}</p>
                          <p className="text-xs text-muted-foreground">ID: {detail.bookId.slice(0, 8)}</p>
                        </div>
                      </div>
                    ))}
                  </div>

                  {request.approverId && (
                    <div className="mt-3 pt-3 border-b text-xs text-muted-foreground">
                      {request.status === RequestStatus.Approved ?
                        `Approved by ${request.approverName || 'Unknown'}` :
                        `Rejected by ${request.approverName || 'Unknown'}`}
                    </div>
                  )}
                </div>
              )}
            </div>
          ))}
        </div>
      )}
    </div>
  );
};

export default BorrowingRequests;