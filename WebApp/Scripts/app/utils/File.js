Ext.define('WEBAPP.utils.File', {
    alternateClassName: ['FileUtil'],
    singleton: true,

    /**
     * @description 當使用者選取檔案後進行的操作
     * @param {file} target file物件
     * @returns {args.fileEl} file物件
     * @returns {args.fileExtValid} 副檔名是否通過檢核
     */
    onFileChange: function (target) {
        var fileExtValid = true;
        var reportTarget = Ext.get(target.reportId).component;
        /*
        if (target.value.length == 0) {
            btnUpload.setText('上傳');
            btnUpload.setDisabled(true);
        } else {
            var file_ext_valid = true;
    
            var len = target.files.length;
            for (i = 0; i < len; i++) {
                var fn = target.files[i].name;
                var di = fn.lastIndexOf(".");
                if (di < 0) {
                    file_ext_valid = false;
                }
                else {
                    var ext = fn.substring(di + 1).toUpperCase();
                    file_ext_valid = (ec_arr.indexOf(ext) > -1);
                }
    
                if (!file_ext_valid) {
                    var w = Ext.Msg.show({
                        title: '上傳檔案副檔名有誤',
                        msg: '無法上傳 ' + fn + ', 須為:' + ec_msg_red + '。',
                        buttons: Ext.Msg.OK,
                        icon: Ext.MessageBox.WARNING,
                        cls: 'warnMsg'
                    });
                    parent.$('#DivResult')[0].innerText = '無法上傳 ' + fn + ', 須為:' + ec_msg + '。';
                    break;
                }
            }
        */

        reportTarget.onFileChange({
            fileEl: target,
            fileExtValid: fileExtValid
        });
    },

    /**
     * @description 上傳檔案
     * @param {boolean} args.isUploadKeyConfirmed uploadKey是否已為已確認
     * @param {guid} args.uploadKey 上傳使用的key
     * @param {file} args.fileField grid中的file field
     * @param {funcion} args.doneCallback 上傳完成後要呼叫的callback
     */
    UploadFiles: function (args) {
        //宣告一個FormData
        var data = new FormData();
        //將檔案append FormData
        var files = args.fileField.files;
        var fLen = files.length;
        if (fLen > 0) {
            for (var i = 0; i < fLen; i++) {
                data.append('[' + i + "].file", files[i]);
            }
        }

        //透過ajax方式Post 至Action
        var ajaxRequest = $.ajax({
            type: "POST",
            url: '/api/File/Upload',
            contentType: false,
            processData: false,
            dataType: "json",
            data: data,
            beforeSend: function (request) {
                //request.setRequestHeader('EC', 'SAP');
                request.setRequestHeader('ST', args.isUploadKeyConfirmed ? 'Y' : 'N');
                request.setRequestHeader('UK', args.uploadKey);
            },
        })
            .done(function (data, textStatus) {
                args.doneCallback(args, { newGuid: data.etts[0], ukConfirmed: data.etts[1] });
            })
            .fail(function (data, textStatus) {
            });
    },

    /**
     * @description 下載檔案(可用於超連結)
     * @param {grid} args.fileGuid 下載檔案的GUID
     */
    DownloadFile: function (args) {
        var fg = args.fileGuid;
        var f = $('<input/>').attr('name', 'FG').attr('value', fg);
        $('<form/>').attr('action', '/api/File/Download')
            .attr('method', 'POST').append(f)
            .appendTo('body').submit().remove();
    },

    /**
     * @description 刪除檔案
     * @param {grid} args.selectedFileGuids 要刪除的檔案列表
     * @param {funcion} args.doneCallback 刪除完成後要呼叫的callback
     */
    DeleteFiles: function (args) {
        var ajaxRequest = $.ajax({
            type: "POST",
            url: '/api/File/Delete',
            dataType: "json",
            data: { FG: args.selectedFileGuids.join(',') }
        })
            .done(function (data, textStatus) {
                args.doneCallback();
            })
            .fail(function (data, textStatus) {
            });
    },

    /**
     * @description 取得新的GUID，如果Submit時，UploadKey還是空白，可使用此值
     * @param {attachment} args.attachmentField 要傳入doneCallback的附件欄位
     * @param {function} args.doneCallback 執行完ajaxRequest後要執行的callback
     */
    NewGuid: function (args) {
        var ajaxRequest = $.ajax({
            type: "POST",
            url: '/api/File/NewGuid',
            dataType: "json"
        })
            .done(function (data, textStatus) {
                args.doneCallback(args, { newGuid: data.etts[0] });
            })
            .fail(function (data, textStatus) {
                return null;
            });
    },

    /**
     * @description 放大瀏覽圖片
     * @param {fileGuid} args.fileGuid 要瀏覽的圖片guid
     */
    ViewImage: function (args) {
        var imageWindow = Ext.create('Ext.Window', {
            renderTo: Ext.getBody(),
            modal: true,
            layout: 'fit',
            autoScroll: true,
            closeAction: 'destroy',
            constrain: true,
            resizable: false,
            closable: true,
            title: '圖片檢視',
            viewport: function () { this.up('viewport'); },
            items: [Ext.create('Ext.Img', {
                src: '/api/File/DownloadImage/' + args.fileGuid
            })]
        });

        imageWindow.show();
    }
});
