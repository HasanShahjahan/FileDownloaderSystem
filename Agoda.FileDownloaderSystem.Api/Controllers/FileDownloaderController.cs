﻿using Agoda.FileDownloaderSystem.DataObjects.Abstraction;
using Agoda.FileDownloaderSystem.DataObjects.Enums;
using Agoda.FileDownloaderSystem.DataObjects.Profiles;
using Agoda.FileDownloaderSystem.Domain.Interfaces;
using Agoda.FileDownloaderSystem.Validation.Validators;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Agoda.FileDownloaderSystem.Api.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class FileDownloaderController : ControllerBase
    {
        private readonly IDownloader _fileDownloader;
        private readonly IFileManager _fileManager;
        private readonly IErrorMapper _errorMapper;
        public FileDownloaderController(IDownloader fileDownloader, IFileManager fileManager,IErrorMapper errorMapper)
        {
            _fileDownloader = fileDownloader;
            _fileManager = fileManager;
            _errorMapper = errorMapper;
        }
        [HttpPost]
        public async Task<IActionResult> Post([FromBody] DataObjects.Domain.File model)
        {
            var response = ServerResponse.OK;
            if (string.IsNullOrEmpty(model.Source)) return Ok(_errorMapper.MapToError(ServerResponse.BadRequest,Resource.EmptySourceUrl));

            string protocol = _fileManager.GetProtocolFromSource(model.Source);
            if(string.IsNullOrEmpty(protocol)) return Ok(_errorMapper.MapToError(ServerResponse.BadRequest, Resource.InvalidProtocol));
            
            var isValidSourceUrl = RequestValidator.CheckURLValid(model.Source, protocol);
            if (!isValidSourceUrl) return Ok(_errorMapper.MapToError(ServerResponse.BadRequest, Resource.InvalidSourceUrl));

            switch (protocol)
            {
                case Protocol.Http:
                    response = await _fileDownloader.DownloadDataFromHttpToLocalDisk(model.Source,protocol);
                    break;
                case Protocol.Https:
                    response = await _fileDownloader.DownloadDataFromHttpsToLocalDisk(model.Source, protocol);
                    break;
                case Protocol.ftp:
                    response = await _fileDownloader.DownloadDataFromFtpToLocalDisk(model.Source, protocol);
                    break;
                case Protocol.sftp:
                    response = await _fileDownloader.DownloadDataFromSftpToLocalDisk(model.Source,protocol);
                    break;
                case Protocol.tcp:
                    response = await _fileDownloader.DownloadDataFromTcpToLocalDisk(model.Source, protocol);
                    break;
                case Protocol.pipe:
                    response = await _fileDownloader.DownloadDataFromPipeToLocalDisk(model.Source, protocol);
                    break;
            }
            return Ok(response);
        }

        [HttpGet]
        public IEnumerable<DataObjects.Domain.File> Get()
        {
            var result = _fileManager.GetAllFileDetails();
            return result;
        }

    }
}
